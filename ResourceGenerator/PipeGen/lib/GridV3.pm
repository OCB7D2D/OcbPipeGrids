use strict;
use warnings;
package GridV3;
use POSIX qw(fmod);
use Math::Vector::Real;

sub V2GridParam
{
	return $_[0] < 0
		? $_[0] - fmod($_[0] - $_[1]/2, $_[1]) - $_[1]/2
		: $_[0] - fmod($_[0] + $_[1]/2, $_[1]) + $_[1]/2;
}

sub V3Grid
{
	return V(
		V2GridParam($_[0]->[0], $_[1]),
		V2GridParam($_[0]->[1], $_[1]),
		V2GridParam($_[0]->[2], $_[1]));
}

sub V3Key
{
	return sprintf("%0.6f %0.6f %0.6f",
		$_[0]->[0], $_[0]->[1], $_[0]->[2]);
}

sub new {
	die "no delta" unless $_[1];
	bless {
		delta => $_[1],
		count => 0,
		list => [],
		grid => {},
		lookup => {},
		vertices => {},
	}, $_[0];
}

sub Add
{
	my $self = shift;
	foreach my $v (@_)
	{
		my $vg = V3Grid($v, $self->{delta});
		my $key = V3Key($vg);
		if (exists $self->{grid}->{$key})
		{
			$v->[3] =
			$self->{lookup}->{$v} =
				$self->{grid}->{$key};
			push @{$self->{vertices}->{$key}}, $v;
		}
		else
		{
			$v->[3] =
			$self->{lookup}->{$v} =
			$self->{grid}->{$key} =
				$self->{count} ++;
			push @{$self->{list}}, $vg;
			$self->{vertices}->{$key} = [$v];
		}
	}
}

sub GetVertices
{
	my ($self, $v) = @_;
	my $bg = V3Key(V3Grid($v, $self->{delta}));
	return @{$_[0]->{vertices}->{$bg}};
}

1;